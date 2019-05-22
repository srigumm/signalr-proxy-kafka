namespace Api.Models{
    public class OrderRequest{
       public int Id { get; set; }
       public string Flavour { get; set; }
       public ItemSize Size { get; set; }
       public int Quantity { get; set; }
    }
    public enum ItemSize{S,M,L,XL}
}