using Amazon.S3;

namespace PrintClassInstanceLib.Messages
{
    public class S3UploadMessage
    {
        public byte[] ByteArray { get; set; }
        public AmazonS3Client S3Client { get; set; }
        public string BucketName { get; set; }
        public string Key { get; set; }
    }
}
