export interface IFeedback {
    feedbacks_Id: number;
    feedbacks_Long_Description: string;
    status: 'negative' | 'positive';
}

export interface IFeedbacksResponse {
    positiveFeedback: IFeedback;
    negativeFeedback: IFeedback;
}