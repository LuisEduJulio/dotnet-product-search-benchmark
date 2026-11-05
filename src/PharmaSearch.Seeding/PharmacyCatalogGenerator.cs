using System.Globalization;
using PharmaSearch.Domain.Products;

namespace PharmaSearch.Seeding;

/// <summary>
/// Gera um catálogo fictício de farmácia, sempre igual para a mesma semente.
/// Marcas e produtos são inventados; os princípios ativos são nomes genéricos.
/// </summary>
public static class PharmacyCatalogGenerator
{
    public const int DefaultSeed = 42;

    public const string Analgesics = "Analgésicos e Antitérmicos";
    public const string Antiallergics = "Antialérgicos";
    public const string Vitamins = "Vitaminas e Suplementos";
    public const string Gastro = "Gastrointestinal";
    public const string Respiratory = "Respiratório";
    public const string Dermo = "Dermocosméticos";
    public const string Hygiene = "Higiene Pessoal";

    private sealed record Ingredient(string Name, string Category, string[] Doses, string[] Forms, string Indication);

    private static readonly Ingredient[] Ingredients =
    [
        new("Dipirona Monoidratada", Analgesics, ["500mg", "1g", "500mg/ml"], ["Comprimido", "Gotas", "Solução Oral"], "dor e febre"),
        new("Paracetamol", Analgesics, ["500mg", "750mg", "200mg/ml"], ["Comprimido", "Gotas", "Suspensão"], "dor e febre"),
        new("Ibuprofeno", Analgesics, ["200mg", "400mg", "600mg", "50mg/ml"], ["Comprimido", "Cápsula", "Gotas"], "dor, febre e inflamação"),
        new("Ácido Acetilsalicílico", Analgesics, ["100mg", "500mg"], ["Comprimido"], "dor e prevenção cardiovascular"),
        new("Nimesulida", Analgesics, ["100mg"], ["Comprimido", "Gotas"], "inflamação e dor"),
        new("Diclofenaco Sódico", Analgesics, ["50mg", "1%"], ["Comprimido", "Gel"], "dor muscular"),
        new("Loratadina", Antiallergics, ["10mg", "1mg/ml"], ["Comprimido", "Xarope"], "rinite alérgica"),
        new("Desloratadina", Antiallergics, ["5mg", "0,5mg/ml"], ["Comprimido", "Xarope"], "alergias respiratórias"),
        new("Cetirizina", Antiallergics, ["10mg"], ["Comprimido", "Gotas"], "urticária e rinite"),
        new("Dexclorfeniramina", Antiallergics, ["2mg", "0,4mg/ml"], ["Comprimido", "Xarope"], "reações alérgicas"),
        new("Fexofenadina", Antiallergics, ["120mg", "180mg"], ["Comprimido"], "rinite alérgica sazonal"),
        new("Vitamina C", Vitamins, ["500mg", "1g", "200mg/ml"], ["Comprimido Efervescente", "Gotas", "Cápsula"], "reforço da imunidade"),
        new("Vitamina D3", Vitamins, ["1.000UI", "2.000UI", "7.000UI"], ["Cápsula", "Gotas"], "saúde óssea"),
        new("Complexo B", Vitamins, ["Alta Potência"], ["Comprimido", "Cápsula"], "metabolismo energético"),
        new("Ômega 3", Vitamins, ["1g"], ["Cápsula"], "saúde cardiovascular"),
        new("Magnésio Dimalato", Vitamins, ["300mg"], ["Cápsula", "Pó"], "função muscular"),
        new("Zinco Quelato", Vitamins, ["30mg"], ["Cápsula"], "imunidade"),
        new("Omeprazol", Gastro, ["20mg", "40mg"], ["Cápsula"], "azia e refluxo"),
        new("Pantoprazol", Gastro, ["20mg", "40mg"], ["Comprimido"], "gastrite e refluxo"),
        new("Simeticona", Gastro, ["40mg", "75mg/ml"], ["Comprimido", "Gotas"], "gases"),
        new("Bromoprida", Gastro, ["10mg", "4mg/ml"], ["Comprimido", "Gotas"], "náusea e vômito"),
        new("Loperamida", Gastro, ["2mg"], ["Comprimido"], "diarreia aguda"),
        new("Lactulose", Gastro, ["667mg/ml"], ["Xarope"], "constipação"),
        new("Ambroxol", Respiratory, ["30mg", "6mg/ml"], ["Xarope", "Gotas"], "tosse com catarro"),
        new("Acetilcisteína", Respiratory, ["600mg", "20mg/ml"], ["Granulado", "Xarope"], "secreção pulmonar"),
        new("Dropropizina", Respiratory, ["3mg/ml"], ["Xarope"], "tosse seca"),
        new("Cloreto de Sódio", Respiratory, ["0,9%"], ["Solução Nasal", "Spray Nasal"], "higiene nasal"),
        new("Guaifenesina", Respiratory, ["13,33mg/ml"], ["Xarope"], "tosse produtiva"),
    ];

    private static readonly (string Product, string Category, string Indication)[] NonMedicines =
    [
        ("Protetor Solar Facial FPS 50", Dermo, "proteção diária contra raios UVA e UVB"),
        ("Protetor Solar Corporal FPS 30", Dermo, "proteção solar para o corpo"),
        ("Hidratante Corporal Ureia 10%", Dermo, "pele extra seca"),
        ("Sérum Vitamina C Facial", Dermo, "uniformizar o tom da pele"),
        ("Gel de Limpeza Facial Pele Oleosa", Dermo, "controle da oleosidade"),
        ("Água Micelar", Dermo, "remoção de maquiagem"),
        ("Shampoo Anticaspa", Hygiene, "controle da caspa"),
        ("Condicionador Hidratação Intensa", Hygiene, "cabelos secos"),
        ("Creme Dental Branqueador", Hygiene, "dentes mais brancos"),
        ("Enxaguante Bucal sem Álcool", Hygiene, "hálito fresco"),
        ("Desodorante Aerosol 48h", Hygiene, "proteção contra o suor"),
        ("Sabonete Líquido Antisséptico", Hygiene, "higiene das mãos"),
        ("Fio Dental Menta", Hygiene, "limpeza entre os dentes"),
        ("Fralda Infantil Tamanho G", Hygiene, "cuidado do bebê"),
    ];

    private static readonly string[] Brands =
        ["Vivere", "Sanavita", "Bioforte", "Pharmaluz", "Nativa Farma", "Cuidar+", "Equilíbrio", "Lab Aurora", "Prevenir", "Genérico"];

    private static readonly string[] UnitPacks = ["10 unidades", "20 unidades", "30 unidades", "60 unidades"];
    private static readonly string[] VolumePacks = ["15ml", "20ml", "100ml", "120ml", "200ml"];
    private static readonly string[] LiquidForms = ["Gotas", "Xarope", "Solução", "Suspensão", "Spray", "Gel"];

    private static string PickPack(Random random, string form) =>
        LiquidForms.Any(f => form.StartsWith(f, StringComparison.Ordinal))
            ? VolumePacks[random.Next(VolumePacks.Length)]
            : UnitPacks[random.Next(UnitPacks.Length)];

    public static IReadOnlyList<Product> Generate(int count, int seed = DefaultSeed)
    {
        var random = new Random(seed);
        var products = new List<Product>(count);

        for (var i = 1; i <= count; i++)
        {
            var sku = $"FRM-{i:D7}";
            var brand = Brands[random.Next(Brands.Length)];
            var price = Math.Round((decimal)(random.NextDouble() * 120 + 4.9), 2);

            if (random.NextDouble() < 0.78)
            {
                var ingredient = Ingredients[random.Next(Ingredients.Length)];
                var dose = ingredient.Doses[random.Next(ingredient.Doses.Length)];
                var form = ingredient.Forms[random.Next(ingredient.Forms.Length)];
                var pack = PickPack(random, form);
                var name = $"{ingredient.Name} {dose} {form} {pack}";

                var description = $"{form} de {ingredient.Name.ToLower(CultureInfo.GetCultureInfo("pt-BR"))} {dose}, " +
                                  $"indicado para {ingredient.Indication}. Embalagem com {pack}.";

                // ~10% citam outro princípio ativo na descrição: ruído realista que testa a relevância.
                if (random.NextDouble() < 0.10)
                {
                    var other = Ingredients[random.Next(Ingredients.Length)];
                    if (other != ingredient)
                        description += $" Alternativa para quem não pode usar {other.Name.ToLower(CultureInfo.GetCultureInfo("pt-BR"))}.";
                }

                products.Add(Product.Create(sku, name, ingredient.Name, brand, ingredient.Category, description, price));
            }
            else
            {
                var item = NonMedicines[random.Next(NonMedicines.Length)];
                var pack = PickPack(random, item.Category == Dermo ? "Gel" : "Unidade");
                var name = $"{item.Product} {brand} {pack}";
                var description = $"{item.Product} indicado para {item.Indication}.";
                products.Add(Product.Create(sku, name, string.Empty, brand, item.Category, description, price));
            }
        }

        return products;
    }
}
