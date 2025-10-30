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

    public monthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
    public daysOfMonth = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];
    public yearsList = Array.from({ length: 10 }, (_, i) => (new Date()).getFullYear() - i)
        .reduce((prev, cur) => { prev[cur] = cur; return prev; }, {})
        ;

    public daysLastMonth = [];
    public daysThisMonth = [];
    public daysNextMonth = [];

    public lastMonthIndex = 0;
    public nextMonthIndex = 0;
    public nextYear = 0;
    public lastYear = 0;

    public selectedYear: number = new Date().getFullYear();
    public selectedMonth: number = new Date().getMonth();

    private _isShown: boolean = false;

    constructor() {
        /* TODO */
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
        this.currentDate = new Date(this.nextYear, this.nextMonthIndex, 1);
        this.selectedMonth = this.currentDate.getMonth();
        this.calc();
    }
    public prevMonth(event) {
        event.stopPropagation();
        this.currentDate = new Date(this.lastYear, this.lastMonthIndex, 1);
        this.selectedMonth = this.currentDate.getMonth();
        this.calc();
    }

    public ngOnInit() {
        /* TODO */
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
    }
}
