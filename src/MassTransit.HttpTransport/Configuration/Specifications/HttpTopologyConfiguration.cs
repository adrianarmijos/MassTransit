﻿// Copyright 2007-2017 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.HttpTransport.Specifications
{
    using EndpointSpecifications;
    using GreenPipes;
    using MassTransit.Topology.Configuration;
    using MassTransit.Topology.Observers;
    using MassTransit.Topology.Topologies;
    using Transports.InMemory.Topology.Configurators;
    using Transports.InMemory.Topology.Topologies;


    public class HttpTopologyConfiguration :
        IHttpTopologyConfiguration
    {
        readonly InMemoryConsumeTopology _consumeTopology;
        readonly IMessageTopologyConfigurator _messageTopology;
        readonly IInMemoryPublishTopologyConfigurator _publishTopology;
        readonly ConnectHandle _publishToSendTopologyHandle;
        readonly ISendTopologyConfigurator _sendTopology;

        public HttpTopologyConfiguration(IMessageTopologyConfigurator messageTopology)
        {
            _messageTopology = messageTopology;

            _sendTopology = new SendTopology();
            _sendTopology.Connect(new DelegateSendTopologyConfigurationObserver(GlobalTopology.Send));

            _publishTopology = new InMemoryPublishTopology(messageTopology);
            _publishTopology.Connect(new DelegatePublishTopologyConfigurationObserver(GlobalTopology.Publish));

            var observer = new PublishToSendTopologyConfigurationObserver(_sendTopology);
            _publishToSendTopologyHandle = _publishTopology.Connect(observer);

            _consumeTopology = new InMemoryConsumeTopology(messageTopology);
        }

        public HttpTopologyConfiguration(IHttpTopologyConfiguration topologyConfiguration)
        {
            _messageTopology = topologyConfiguration.Message;
            _sendTopology = topologyConfiguration.Send;
            _publishTopology = topologyConfiguration.Publish;

            _consumeTopology = new InMemoryConsumeTopology(topologyConfiguration.Message);
        }

        IMessageTopologyConfigurator ITopologyConfiguration.Message => _messageTopology;
        ISendTopologyConfigurator ITopologyConfiguration.Send => _sendTopology;
        IPublishTopologyConfigurator ITopologyConfiguration.Publish => _publishTopology;
        IConsumeTopologyConfigurator ITopologyConfiguration.Consume => _consumeTopology;

        IInMemoryPublishTopologyConfigurator IHttpTopologyConfiguration.Publish => _publishTopology;
        IInMemoryConsumeTopologyConfigurator IHttpTopologyConfiguration.Consume => _consumeTopology;

        public void SeparatePublishFromSendTopology()
        {
            _publishToSendTopologyHandle?.Disconnect();
        }
    }
}