namespace BlueprintRepository.Models {
    public sealed class GenericResponseModel {
        public string ResponseCode { get; set; }
        public string Message { get; set; }
        public object Errors { get; set; }
        public object Data { get; set; }
    }
}
