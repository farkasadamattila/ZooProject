namespace Zoo
{
    internal class Animal
    {
        public int id { get; set; }
        public string species { get; set; }
        public string habitat { get; set; }
        public string arrival_date { get; set; }
        public string health_status { get; set; } = "Good";
        public bool is_fed { get; set; } = true;
    }
}
