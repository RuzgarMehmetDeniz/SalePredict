public class BinaryClassificationResultDto
{
    public double Accuracy { get; set; }
    public double Precision { get; set; }
    public double Recall { get; set; }
    public double F1Score { get; set; }
    public double AUC { get; set; }
    public List<ProductPredictionResultDto> Predictions { get; set; }
}

public class ProductPredictionResultDto
{
    public string ProductName { get; set; }
    public bool PredictedLabel { get; set; }
    public float Probability { get; set; }
}