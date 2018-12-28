export class Helper {
    static roundNumber(number: number, precision: number): number {
        return Math.floor(number * Math.pow(10, precision)) / Math.pow(10, precision);
    }

    static getNthOrLastIndexOf(text: string, substring: string, n: number): number {
        let times: number = 0;
        let index: number = -1;
        let currentIndex: number = -1;

        while (times < n) {
            currentIndex = text.indexOf(substring, index + 1);
            if (currentIndex === -1) {
                break;
            } else {
                index = currentIndex;
            }

            times++;
        }

        return index;
    }
}