using System;

namespace PrintClassInstanceLib.Model
{
    public class StackedObject
    {
        public Guid Id { get; set; }
        public object Obj { get; set; }
        public Guid ParentId { get; set; }
    }
}