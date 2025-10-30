import { SendingMethod } from './contact.model';

export class SelectDistinctContactVideoConfrence
{
    public selected:boolean;    
    public contactId: string;
    public sendingMethod: SendingMethod;
    public contactMeans: string;
    public contactName: string;
    public phoneExtension: string;

}

export class VideoConfrenceResponseDTO
{
    public conferenceHostUrl : string;
}


export class VideoConfrenceRequestDTO
{
    public documentCollectionName : string;
    public VideoConferenceUsers : SelectContactVideoConfrenceDTO[];

}

export class SelectContactVideoConfrenceDTO
{
    public sendingMethod: SendingMethod;
    public means: string;
    public fullName: string;
    public phoneExtension: string;

}