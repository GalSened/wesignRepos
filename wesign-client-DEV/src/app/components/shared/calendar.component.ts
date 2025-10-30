import { Component, EventEmitter, Input, OnInit, Output } from "@angular/core";

@Component({
    selector: "sgn-calendar-component",
    templateUrl: "calendar.component.html",
})

export class CalendarComponent implements OnInit {

    public get isShown(): boolean {
        return this._isShown;
    }

    @Input()
    public set isShown(value: boolean) {
        this._isShown = value;
        this.calc();
    }

    public now: Date = new Date();

    @Input()
    public selectedDate: Date = new Date();

    public currentDate: Date = new Date();

    @Output()
    public selected: EventEmitter<Date> = new EventEmitter();

    public monthNames = {0:"Jan",1: "Feb",2: "Mar",3: "Apr",4: "May",5: "Jun",6: "Jul",7: "Aug",8: "Sep",9: "Oct",10: "Nov",11: "Dec"};
    public daysOfMonth = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];
    public yearsList = Array.from({ length: 10 }, (_, i) => (new Date()).getFullYear() - i)
        .reduce((prev, cur) => { prev[cur] = cur; return prev; }, {})
        ;

    public isAfterPastLimit = false;
    public isInTheFuture = true;

    public daysInTheFuture = [];


    public daysLastMonth = [];
    public daysThisMonth = [];
    public daysNextMonth = [];

    public lastMonthIndex = 0;
    public nextMonthIndex = 0;
    public nextYear = 0;
    public lastYear = 0;

    public selectedYear: number = new Date().getFullYear();
    public selectedMonth: number = new Date().getMonth();

    public presentMonth: number = new Date().getMonth();
    public presentYear: number = new Date().getFullYear();

    private _isShown: boolean = false;

    constructor() {

    }

    public selectPrevMonthDate(date: number) {
        this.currentDate = new Date(this.lastYear, this.lastMonthIndex, date);
        this.isShown = false;
        this.selected.emit(this.currentDate);
    }

    public selectNextMonthDate(date: number) {
        this.currentDate = new Date(this.nextYear, this.nextMonthIndex, date);
        this.isShown = false;
        this.selected.emit(this.currentDate);
    }

    public selectMonthDate(date: number) {
        this.currentDate = new Date(this.currentDate.getFullYear(), this.currentDate.getMonth(), date);
        this.isShown = false;
        this.selected.emit(this.currentDate);
    }

    public nextMonth(event) {
        event.stopPropagation();
        let dayDate = this.dayOfMonth(this.nextMonthIndex)
        this.currentDate = new Date(this.nextYear, this.nextMonthIndex, dayDate);
        this.selectedMonth = this.currentDate.getMonth();
        this.selectedYear = this.currentDate.getFullYear();
        this.calc();
    }
    public prevMonth(event) {
        event.stopPropagation();
        let dayDate = this.dayOfMonth(this.lastMonthIndex)
        this.currentDate = new Date(this.lastYear, this.lastMonthIndex, dayDate);
        this.selectedMonth = this.currentDate.getMonth();
        this.selectedYear = this.currentDate.getFullYear();
        this.calc();

    }
    public dayOfMonth(month: number) {
        if (month == new Date().getMonth()) {
            return (new Date().getDate());
        }
        return 1;
    }

    public ngOnInit() {
        let language = document.getElementsByTagName("html")[0].getAttribute("dir");
        let isEng = language == "ltr";

        if (!isEng) {
            this.monthNames = {0:"ינו",1: "פבר",2: "מרץ",3: "אפר",4: "מאי",5: "יונ",6: "יול",7: "אוג",8: "ספט",9: "אוק",10: "נוב",11: "דצמ"};
            
        }
    }

    public yearChanged() {
        this.currentDate = new Date(this.selectedYear, this.currentDate.getMonth(), this.currentDate.getDate());
        this.calc();
    }

    public monthChanged() {
        this.currentDate = new Date(this.currentDate.getFullYear(), this.selectedMonth, this.currentDate.getDate());
        this.calc();
    }

    private calc() {

        this.daysLastMonth = [];
        this.daysThisMonth = [];
        this.daysNextMonth = [];

        this.daysOfMonth[1] = (this.currentDate.getFullYear() % 4 === 0) ? 29 : 28;

        this.lastMonthIndex = (this.currentDate.getMonth() - 1 + 12) % 12;
        this.nextMonthIndex = (this.currentDate.getMonth() + 1) % 12;

        this.lastYear = (this.lastMonthIndex === 11) ?
            (this.currentDate.getFullYear() - 1) : this.currentDate.getFullYear();
        this.nextYear = (this.nextMonthIndex === 0) ?
            (this.currentDate.getFullYear() + 1) : this.currentDate.getFullYear();

        const lastDayLastMonth =
            new Date(this.lastYear, this.lastMonthIndex, this.daysOfMonth[this.lastMonthIndex]).getDay();

        const lastDayThisMonth =
            new Date(this.currentDate.getFullYear(), this.currentDate.getMonth(),
                this.daysOfMonth[this.currentDate.getMonth()]).getDay();



        for (let i = 1; i < lastDayLastMonth + 1; i++) {
            this.daysLastMonth.push(
                this.daysOfMonth[this.lastMonthIndex] - (lastDayLastMonth - i),
            );
        }

        for (let i = 1; i <= this.daysOfMonth[this.currentDate.getMonth()]; i++) {
            this.daysThisMonth.push(i);
        }

        if (lastDayThisMonth !== 0) {

            for (let i = 1; i <= (7 - lastDayThisMonth); i++) {
                this.daysNextMonth.push(i);
            }
        }

        this.isInTheFuture = (this.selectedMonth == new Date().getMonth());
        this.isAfterPastLimit = (this.selectedMonth == 0 && this.selectedYear == new Date().getFullYear() - Object.keys(this.yearsList).length + 1);

        if (this.isInTheFuture) {
            this.getDaysInTheFuture();
        }
        else {
            this.daysInTheFuture.length = 0;
        }
    }

    private getDaysInTheFuture() {
        this.daysInTheFuture.length = 0;
        let currentDateDay = new Date().getDate() + 1;
        let lastDateOfTheMonth = new Date(this.currentDate.getFullYear(), this.currentDate.getMonth(), this.daysOfMonth[this.currentDate.getMonth()]).getDate();
        for (let i = currentDateDay; i <= lastDateOfTheMonth; i++) {
            this.daysInTheFuture.push(i);
        }
    }
}
