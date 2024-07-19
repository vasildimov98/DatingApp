import { Component, inject, input, OnInit, output } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';
import {
  AbstractControl,
  FormBuilder,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { JsonPipe, NgIf } from '@angular/common';
import { TextInputComponent } from '../_forms/text-input/text-input.component';
import { DatePickerComponent } from '../_forms/date-picker/date-picker.component';
import { Router } from '@angular/router';
import { daLocale } from 'ngx-bootstrap/chronos';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    JsonPipe,
    NgIf,
    TextInputComponent,
    DatePickerComponent,
  ],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css',
})
export class RegisterComponent implements OnInit {
  private fb = inject(FormBuilder);
  private accountService = inject(AccountService);
  private router = inject(Router);

  cancelRegister = output<boolean>();
  registerForm: FormGroup = new FormGroup({});
  maxDate = new Date();
  validationErrors: string[] | undefined;

  ngOnInit(): void {
    this.initializeFormGroup();
    this.maxDate.setFullYear(this.maxDate.getFullYear() - 18);
  }

  private initializeFormGroup() {
    this.registerForm = this.fb.group({
      gender: ['male'],
      username: ['', Validators.required],
      knownAs: ['', Validators.required],
      dateOfBirth: ['', Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      password: [
        '',
        [Validators.required, Validators.minLength(4), Validators.maxLength(8)],
      ],
      confirmPassword: [
        '',
        [Validators.required, this.matchValues('password')],
      ],
    });

    this.registerForm.controls['password'].valueChanges.subscribe({
      next: () => {
        this.registerForm.controls['confirmPassword'].updateValueAndValidity();
      },
    });
  }

  matchValues(matchTo: string): ValidatorFn {
    return (control: AbstractControl) => {
      return control.value === control.parent?.get(matchTo)?.value
        ? null
        : { isMatching: true };
    };
  }

  register() {
    var dateOfBirth = this.getDateOnly(
      this.registerForm.get('dateOfBirth')?.value
    );
    console.log(dateOfBirth);
    this.registerForm.patchValue({ dateOfBirth });
    this.accountService.register$(this.registerForm.value).subscribe({
      next: (_) => this.router.navigateByUrl('/members'),
      error: (errors) => (this.validationErrors = errors),
    });
  }

  cancel() {
    this.cancelRegister.emit(false);
  }

  private getDateOnly(dateOfBirth: string | undefined) {
    if (!dateOfBirth) return;

    return new Date(dateOfBirth).toISOString().slice(0, 10);
  }
}
