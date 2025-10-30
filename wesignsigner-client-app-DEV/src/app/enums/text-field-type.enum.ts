export enum TextFieldType {
    None = 0,
    Text = 1, // 
    Date = 2, // date //"^([0-2][0-9]|(3)[0-1])(\/)(((0)[0-9])|((1)[0-2]))(\/)\d{4}$"
    Number = 3,// number //"^[0-9]*$"
    Phone = 4, // number
    Email = 5, // isEmail // "^[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}$"
    Custom = 6, // pattern
    Time = 7, //(((2[0-3])|(1[0-9])|0[1-9]):([0-5][0-9])) -> HH:MM placeholder
    List = 8,
    Multiline = 9
}