using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ComX.Infrastructure.Distributed.Outbox.Tests
{
    public interface IEventOne 
    {
        public Guid ReferenceId { get;  }
        public string ReferenceType { get; }
        public Guid DocumentId { get; }
        public string FileName { get; }

        /// <summary>
        /// If the document belongs to an archive/eml the path location
        /// describes an hierarchical path to the file.
        /// Note: an eml can contain multiple documents with the same name,
        /// and in this case PathLocation will be the same
        /// E.G. archive_with_directory.7z/content/sample.7z/sample.eml
        /// E.G. archive_with_directory.7z/content/sample.7z/sample.eml/background.gif
        /// </summary>
        public string PathLocation { get; }
        public string MimeType { get; }
        public string FileExtension { get; }
    }
}
