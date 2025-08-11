import { Component } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { AuthService } from '@core/services/auth.service';

@Component({
  selector: 'app-login-page',
  template: `
  <div class="container py-5">
    <div class="row justify-content-center">
      <div class="col-12 col-sm-10 col-md-8 col-lg-5">
        <div class="card shadow-sm">
          <div class="card-body p-4">
            <div class="d-flex align-items-center mb-3 gap-2">
              <img src="assets/brand/dija-gold-logo.svg" alt="Dija Gold" height="40">
              <h5 class="mb-0">Sign in</h5>
            </div>

            <form [formGroup]="form" (ngSubmit)="submit()" class="d-grid gap-3">
              <div>
                <label class="form-label">Username</label>
                <input class="form-control" formControlName="username" required>
              </div>
              <div>
                <label class="form-label">Password</label>
                <input class="form-control" formControlName="password" type="password" required>
              </div>
              <button class="btn btn-dark brand-border" [disabled]="form.invalid">Login</button>
            </form>
          </div>
        </div>
      </div>
    </div>
  </div>
  `
})
export class LoginPageComponent {
  form = this.fb.group({
    username: ['', Validators.required],
    password: ['', Validators.required],
  });

  constructor(private fb: FormBuilder, private auth: AuthService) {}

  submit() {
    if (this.form.invalid) return;
    this.auth.login(this.form.value as any).subscribe();
  }
}


