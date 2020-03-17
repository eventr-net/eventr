namespace EventR.Spec.Serialization
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    [Serializable]
    public class TestSubCase
    {
        [DataMember(Order = 1)]
        public int TheInt { get; set; }

        [DataMember(Order = 2)]
        public string TheString { get; set; }
    }
}
