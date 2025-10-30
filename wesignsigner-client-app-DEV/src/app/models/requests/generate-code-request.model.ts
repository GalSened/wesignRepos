export class GenerateCodeRequest{
    public token : string;
    public identification : string;    
}

export class GenerateCodeResponse{
    public sentSignerMeans : string;
    public authToken : string;
}