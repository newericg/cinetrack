import { Component, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './topbar.component.html',
  styleUrl: './topbar.component.scss',
})
export class TopbarComponent {
  readonly isDarkMode = input.required<boolean>();
  readonly toggleTheme = output<void>();
  readonly searchChange = output<string>();

  searchQuery = '';

  onSearch() {
    this.searchChange.emit(this.searchQuery);
  }
}
