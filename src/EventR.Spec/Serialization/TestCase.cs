namespace EventR.Spec.Serialization
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    [Serializable]
    public class TestCase
    {
        [DataMember(Order = 1)]
        public string TheString { get; set; }

        [DataMember(Order = 2)]
        public string NullString { get; set; }

        [DataMember(Order = 3)]
        public int TheInt { get; set; }

        [DataMember(Order = 4)]
        public int? NullableInt { get; set; }

        [DataMember(Order = 5)]
        public uint TheUInt { get; set; }

        [DataMember(Order = 6)]
        public uint? NullableUInt { get; set; }

        [DataMember(Order = 7)]
        public bool TheBoolean { get; set; }

        [DataMember(Order = 8)]
        public bool? NullableBoolean { get; set; }

        [DataMember(Order = 9)]
        public Guid TheGuid { get; set; }

        [DataMember(Order = 10)]
        public Guid? NullableGuid { get; set; }

        [DataMember(Order = 11)]
        public DateTime DateTime { get; set; }

        [DataMember(Order = 12)]
        public DateTime? NullableDateTime { get; set; }

        [DataMember(Order = 13)]
        public string[] ArrayOfStrings { get; set; }

        [DataMember(Order = 14)]
        public int[] ArrayOfInt { get; set; }

        [DataMember(Order = 15)]
        public TestSubCase SubCase { get; set; }

        [DataMember(Order = 16)]
        public TestSubCase[] ArrayOfSubCases { get; set; }
    }
}
