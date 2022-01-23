using AutoMapper;
using Newtonsoft.Json;
using System;

namespace ComX.Infrastructure.Distributed.Outbox.Tests
{
    public class ProfileCustomMessageLog : Profile
    {
        public ProfileCustomMessageLog()
        {
            CreateMap<IEventOne, CustomMessageLog>()
                .ConstructUsing((Func<IEventOne, ResolutionContext, CustomMessageLog>)((model, _) =>
                {
                    return new CustomMessageLog
                    {
                        Id = Guid.NewGuid(),
                        MessageTypeName = Consts.EVENT_ONE_NAME,
                        MessageBody = JsonConvert.SerializeObject(model),
                        Status = OutboxStatus.NotPublished
                    };
                }));

            CreateMap<IEventTwo, CustomMessageLog>()
                .ConstructUsing((Func<IEventTwo, ResolutionContext, CustomMessageLog>)((model, _) =>
                {
                    return new CustomMessageLog
                    {
                        Id = Guid.NewGuid(),
                        MessageTypeName = Consts.EVENT_TWO_NAME,
                        MessageBody = JsonConvert.SerializeObject(model),
                        Status = OutboxStatus.NotPublished
                    };
                }));
        }
    }
}
