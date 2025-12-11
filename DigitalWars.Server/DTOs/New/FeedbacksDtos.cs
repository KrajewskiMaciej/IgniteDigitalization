namespace DigitalWars.Server.Dtos
{
    public class FeedbackDetailsDto
    {
        public int Feedbacks_Id { get; set; }
        public string Feedbacks_Long_Description { get; set; } = string.Empty;
    }

    public class CardFeedbacksDto
    {
        public FeedbackDetailsDto? PositiveFeedback { get; set; }
        public FeedbackDetailsDto? NegativeFeedback { get; set; }
    }

    public class UpdateCardFeedbacksDto
    {
        public string PositiveDescription { get; set; } = string.Empty;
        public string NegativeDescription { get; set; } = string.Empty;
    }
}