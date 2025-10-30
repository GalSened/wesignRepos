import { Injectable } from "@angular/core";

@Injectable()
export class PagerService {

    public getPager(totalItems: number, currentPage: number = 1, pageSize: number = 10) {
        // calculate total pages
        let totalPages = Math.ceil(totalItems / pageSize);
        totalPages = totalPages < 1 ? 1 : totalPages;

        let startPage: number;
        let endPage: number;

        if (totalPages <= 5) {
            startPage = 1;
            endPage = totalPages;
        } else {
            if (currentPage <= 3) {
                startPage = 1;
                endPage = totalPages;
            } else if (currentPage + 1 >= totalPages) {
                startPage = totalPages - 2;
                endPage = totalPages;
            } else {
                startPage = currentPage - 2;
                endPage = totalPages ;
            }
        }

        currentPage = currentPage <= endPage ? currentPage : 1;

        // calculate start and end item indexes
        const startIndex = (currentPage - 1) * pageSize;
        const endIndex = Math.min(startIndex + pageSize - 1, totalItems - 1);

        // create an array of pages to ng-repeat in the pager control
        const pages = [];
        for (let i = startPage; i <= endPage; i++) {
            pages.push(i);
        }

        // return object with all pager properties required by the view
        return {
            currentPage,
            endIndex,
            endPage,
            pageSize,
            pages,
            startIndex,
            startPage,
            totalItems,
            totalPages,
        };
    }
}
