namespace Domain.Entities
{
    public class ShopList
    {
        public string Id { get; set; }
        public int MeatWeight { get; set; }
        public int VeggiesWeight { get; set; }

        public ShopList(string id)
        {
            Id = id;
        }

        public void Incremet(bool isVeg)
        {
            if (isVeg)
            {
                VeggiesWeight += 600;
            }
            else
            {
                MeatWeight += 300;
                VeggiesWeight += 300;
            }
        }

        public void Decrement(bool isVeg)
        {
            if (isVeg)
            {
                if (VeggiesWeight < 600) VeggiesWeight = 0;
                else VeggiesWeight -= 600;
            }
            else
            {
                if (MeatWeight < 300) MeatWeight = 0;
                else MeatWeight -= 300;

                if (VeggiesWeight < 300) VeggiesWeight = 0;
                else VeggiesWeight -= 300;
            }
        }

        public object? TakeSnapshot()
        {
            return new
            {
                Id,
                VeggiesWeight = GramsToKilogramsConverter(VeggiesWeight),
                MeatWeight = GramsToKilogramsConverter(MeatWeight),
            };
        }

        private double GramsToKilogramsConverter(int grams)
        {
            return grams / 1000;
        }
    }
}
