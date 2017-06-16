using System;
namespace PrintClassInstanceLib.Messages
{
    public class OperationMessage
    {
        public bool Error { get; set; }
        public Exception Ex { get; set; }
        public string ErrorMessage { get; set; }
    }
}
