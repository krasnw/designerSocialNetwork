import { Component } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink],
  templateUrl: `app.component.html`,
  styles: `
    :host {
      display: flex;
      flex-direction: row;
    }
  `,
})
export class AppComponent {}
