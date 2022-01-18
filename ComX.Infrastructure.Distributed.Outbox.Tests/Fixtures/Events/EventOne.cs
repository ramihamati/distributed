using System;

namespace ComX.Infrastructure.Distributed.Outbox.Tests;
public class EventOne : IEventOne
{
    public Guid ReferenceId { get; set; }

    public string ReferenceType { get; set; }

    public Guid DocumentId { get; set; }

    public string FileName { get; set; }

    public string PathLocation { get; set; }

    public string MimeType { get; set; }

    public string FileExtension { get; set; }
}
