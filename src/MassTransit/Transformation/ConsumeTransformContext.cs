// Copyright 2007-2015 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Transformation
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Context;


    /// <summary>
    /// Sits in front of the consume context and allows the inbound message to be 
    /// transformed.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public class ConsumeTransformContext<TMessage> :
        TransformContext<TMessage>
        where TMessage : class
    {
        readonly ConsumeContext<TMessage> _context;
        readonly PayloadCache _payloadCache;

        public ConsumeTransformContext(ConsumeContext<TMessage> context)
        {
            _context = context;
            _payloadCache = new PayloadCache();
        }

        IEnumerable<string> TransformContext.SupportedMessageTypes
        {
            get { return _context.SupportedMessageTypes; }
        }

        CancellationToken TransformContext.CancellationToken
        {
            get { return _context.CancellationToken; }
        }

        bool TransformContext.HasPayloadType(Type contextType)
        {
            return _payloadCache.HasPayloadType(contextType) || _context.HasPayloadType(contextType);
        }

        bool TransformContext.TryGetPayload<TPayload>(out TPayload context)
        {
            if (_payloadCache.TryGetPayload(out context))
                return true;

            return _context.TryGetPayload(out context);
        }

        TPayload TransformContext.GetOrAddPayload<TPayload>(PayloadFactory<TPayload> payloadFactory)
        {
            TPayload payload;
            if (_payloadCache.TryGetPayload(out payload))
                return payload;

            if (_context.TryGetPayload(out payload))
                return payload;

            return _payloadCache.GetOrAddPayload(payloadFactory);
        }

        TMessage TransformContext<TMessage>.Input
        {
            get { return _context.Message; }
        }

        public bool HasInput
        {
            get { return true; }
        }

        public async Task<TransformResult<TMessage>> ReturnOriginal()
        {
            return new OriginalResult(_context.Message);
        }

        bool TransformContext.HasMessageType(Type messageType)
        {
            return _context.HasMessageType(messageType);
        }

        bool TransformContext.TryGetMessage<T>(out T message)
        {
            ConsumeContext<T> consumeContext;
            if (_context.TryGetMessage(out consumeContext))
            {
                message = consumeContext.Message;
                return true;
            }

            message = null;
            return false;
        }


        class OriginalResult :
            TransformResult<TMessage>
        {
            readonly TMessage _value;

            public OriginalResult(TMessage value)
            {
                _value = value;
            }

            public TMessage Value
            {
                get { return _value; }
            }

            public bool IsNewValue
            {
                get { return false; }
            }
        }
    }
}