import { HttpClient } from '@angular/common/http';
import { Component, inject } from '@angular/core';

@Component({
  selector: 'app-test-errors',
  standalone: true,
  imports: [],
  templateUrl: './test-errors.component.html',
  styleUrl: './test-errors.component.css'
})
export class TestErrorsComponent {
  private baseUrl = "https://localhost:5001/api/";
  private http = inject(HttpClient);

  validationErrors: string[] = [];

  getValidationError() {
    this.http.post(this.baseUrl + "account/register", {}).subscribe({
      next: response => console.log(response),
      error: error => {
        console.log(error);
        this.validationErrors = error;
      }
    });
  }

  getBadRequestError() {
    this.http.get(this.baseUrl + "buggy/bad-request").subscribe({
      next: response => console.log(response),
      error: error => console.log(error)
    });
  }

  getUnauthorizedError() {
    this.http.get(this.baseUrl + "buggy/auth").subscribe({
      next: response => console.log(response),
      error: error => console.log(error)
    });
  }

  getNotFoundError() {
    this.http.get(this.baseUrl + "buggy/not-found").subscribe({
      next: response => console.log(response),
      error: error => console.log(error)
    });
  }

  getServerError() {
    this.http.get(this.baseUrl + "buggy/server-error").subscribe({
      next: response => console.log(response),
      error: error => console.log(error)
    });
  }
}
