export interface IField { }

export class BaseField implements IField{
    public name : string;
    public description:string;
    public x : number;
    public y: number;
    public width: number;
    public height : number;
    public mandatory: boolean;
    public fieldGroup:number;
    public page :number;
}