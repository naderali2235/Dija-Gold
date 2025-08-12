import { Directive, ElementRef, AfterViewInit, Input } from '@angular/core';

@Directive({
  selector: '[appAutofocus]'
})
export class AutofocusDirective implements AfterViewInit {
  @Input() appAutofocus: boolean = true;

  constructor(private elementRef: ElementRef) {}

  ngAfterViewInit(): void {
    if (this.appAutofocus) {
      // Use setTimeout to ensure the element is fully rendered
      setTimeout(() => {
        this.elementRef.nativeElement.focus();
      }, 100);
    }
  }
}