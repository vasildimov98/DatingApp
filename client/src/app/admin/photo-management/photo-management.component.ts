import { Component, inject, OnInit, signal } from '@angular/core';
import { AdminService } from '../../_services/admin.service';
import { Photo } from '../../_models/photo';

@Component({
  selector: 'app-photo-management',
  standalone: true,
  imports: [],
  templateUrl: './photo-management.component.html',
  styleUrl: './photo-management.component.css',
})
export class PhotoManagementComponent implements OnInit {
  private adminService = inject(AdminService);

  public unapprovedPhotos = signal<Photo[]>([]);

  ngOnInit(): void {
    this.adminService.getPhotosForApproval().subscribe({
      next: (photos: Photo[]) => {
        if (photos !== null) this.unapprovedPhotos.set(photos);
      },
      error: (err) => console.error(err),
    });
  }

  approvePhoto(photoId: number) {
    this.adminService.approvePhoto(photoId).subscribe({
      next: (photos: Photo[]) => {
        if (photos !== null) this.unapprovedPhotos.set(photos);
      },
      error: (err) => console.error(err),
    });
  }

  rejectPhoto(photoId: number) {
    this.adminService.rejectPhoto(photoId.toString()).subscribe({
      next: (photos: Photo[]) => {
        if (photos !== null) this.unapprovedPhotos.set(photos);
      },
      error: (err) => console.error(err),
    });
  }
}
