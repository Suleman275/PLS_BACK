namespace UserManagement.API.Configurations {
    public class S3Options {
        public string BucketName { get; set; } = default!;
        public string AccessKey { get; set; } = default!;
        public string SecretKey { get; set; } = default!;
        public string Region { get; set; } = default!;
    }
}
