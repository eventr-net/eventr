namespace EventR.Spec.Serialization
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    [Serializable]
    public class TestCase
    {
        [DataMember]
        public string TheString { get; set; }

        [DataMember]
        public string NullString { get; set; }

        [DataMember]
        public int TheInt { get; set; }

        [DataMember]
        public int? NullableInt { get; set; }

        [DataMember]
        public uint TheUInt { get; set; }

        [DataMember]
        public uint? NullableUInt { get; set; }

        [DataMember]
        public bool TheBoolean { get; set; }

        [DataMember]
        public bool? NullableBoolean { get; set; }

        [DataMember]
        public Guid TheGuid { get; set; }

        [DataMember]
        public Guid? NullableGuid { get; set; }

        [DataMember]
        public DateTime DateTime { get; set; }

        [DataMember]
        public DateTime? NullableDateTime { get; set; }

        [DataMember]
        public string[] ArrayOfStrings { get; set; }

        [DataMember]
        public int[] ArrayOfInt { get; set; }

        [DataMember]
        public TestSubCase SubCase { get; set; }

        [DataMember]
        public TestSubCase[] ArrayOfSubCases { get; set; }
    }
}
