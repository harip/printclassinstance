using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3.Transfer;
using PrintClassInstanceLib.Messages;

namespace PrintClassInstanceLib.Upload
{
    public class S3Operations
    {
        public static async Task<OperationMessage> UploadToS3(S3UploadMessage uploadMessage)
        {
            try
            {
                var cts = new CancellationTokenSource();
                var transferUtility = new TransferUtility(uploadMessage.S3Client);
                using (Stream ms = new MemoryStream(uploadMessage.ByteArray))
                {
                    await transferUtility.UploadAsync(ms, uploadMessage.BucketName, uploadMessage.Key, cts.Token);
                }
                return new OperationMessage
                {
                    Error = false,
                    Ex = null,
                    ErrorMessage = null
                };
            }
            catch (Exception ex)
            {
                return new OperationMessage
                {
                    Error = true,
                    Ex = ex,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}
