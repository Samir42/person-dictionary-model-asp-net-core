namespace PersonDictionaryModel.Core.Model.Resourcearameters
{
    public sealed class ResourceParameter
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public string FirstName { get; set; }

        public string LastName { get; set; }
        
        public string PersonalNumber { get; set; }
    }
}
