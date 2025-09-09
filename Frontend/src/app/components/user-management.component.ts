import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService } from './user.service';
import { User } from './user.model';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.css']
})
export class UserManagementComponent implements OnInit {
  private userService = inject(UserService);

  users = signal<User[]>([]);
  currentUser: User = { name: '', email: '' };
  editingUser = signal<User | null>(null);
  loading = signal<boolean>(false);
  loadingUsers = signal<boolean>(false);
  error = signal<string>('');

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.loadingUsers.set(true);
    this.error.set('');
    
    this.userService.getUsers().subscribe({
      next: (users) => {
        this.users.set(users);
        this.loadingUsers.set(false);
      },
      error: (error) => {
        this.error.set('Failed to load users: ' + error.message);
        this.loadingUsers.set(false);
      }
    });
  }

  saveUser(): void {
    this.loading.set(true);
    this.error.set('');

    if (this.editingUser()) {
      // Update existing user
      const userId = this.editingUser()!.id!;
      this.userService.updateUser(userId, this.currentUser).subscribe({
        next: () => {
          this.loadUsers();
          this.resetForm();
          this.loading.set(false);
        },
        error: (error) => {
          this.error.set('Failed to update user: ' + error.message);
          this.loading.set(false);
        }
      });
    } else {
      // Create new user
      this.userService.createUser(this.currentUser).subscribe({
        next: () => {
          this.loadUsers();
          this.resetForm();
          this.loading.set(false);
        },
        error: (error) => {
          this.error.set('Failed to create user: ' + error.message);
          this.loading.set(false);
        }
      });
    }
  }

  editUser(user: User): void {
    this.editingUser.set(user);
    this.currentUser = { ...user };
  }

  cancelEdit(): void {
    this.resetForm();
  }

  deleteUser(userId: string): void {
    this.loading.set(true);
    this.error.set('');
    
    this.userService.deleteUser(userId).subscribe({
      next: () => {
        this.loadUsers();
        this.loading.set(false);
      },
      error: (error) => {
        this.error.set('Failed to delete user: ' + error.message);
        this.loading.set(false);
      }
    });
  }

  private resetForm(): void {
    this.currentUser = { name: '', email: '' };
    this.editingUser.set(null);
  }
}
