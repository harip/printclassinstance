using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Transfer;

namespace PrintClassInstanceLib.Uploads
{
    public class S3Operations
    {
        private readonly TransferUtility _transferUtility;

        public S3Operations(IAmazonS3 client, TransferUtility transferUtility = null)
        {
            _transferUtility = transferUtility ?? new TransferUtility(client);
        }

        public Task UploadFile(List<string> filePaths, string bucketName)
        {
            var validFiles = GetValidFiles(filePaths);
            if (!validFiles.Any())
            {
                return null;
            }

            var uploadTask = _transferUtility.UploadAsync(filePath, bucketName);

            return uploadTask;
        }
    }
}
