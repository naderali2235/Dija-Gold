import { Directive, ElementRef, Output, EventEmitter, HostListener } from '@angular/core';

@Directive({
  selector: '[appClickOutside]'
})
export class ClickOutsideDirective {
  @Output() appClickOutside = new EventEmitter<Event>();

  constructor(private elementRef: ElementRef) {}

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event): void {
    const target = event.target as HTMLElement;
    const clickedInside = this.elementRef.nativeElement.contains(target);
    
    if (!clickedInside) {
      this.appClickOutside.emit(event);
    }
  }
}