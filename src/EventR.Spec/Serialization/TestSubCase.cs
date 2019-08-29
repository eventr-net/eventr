namespace EventR.Spec.Serialization
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    [Serializable]
    public class TestSubCase
    {
        [DataMember]
        public int TheInt { get; set; }

        [DataMember]
        public string TheString { get; set; }
    }
}
