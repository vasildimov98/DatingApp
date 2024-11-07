import {
  AfterViewChecked,
  Component,
  ElementRef,
  inject,
  input,
  ViewChild,
  ViewContainerRef,
} from '@angular/core';
import { MessageService } from '../../../_services/message.service';
import { TimeagoModule } from 'ngx-timeago';
import { FormsModule, NgForm } from '@angular/forms';

@Component({
  selector: 'app-member-messages',
  standalone: true,
  imports: [TimeagoModule, FormsModule],
  templateUrl: './member-messages.component.html',
  styleUrl: './member-messages.component.css',
})
export class MemberMessagesComponent implements AfterViewChecked {
  @ViewChild('messageForm') messageForm?: NgForm;
  @ViewChild('scrollMe') scrollContainer?: ElementRef;
  private messageService = inject(MessageService);

  messages = this.messageService.messageThread;
  username = input.required<string>();
  messageContent = '';
  previousMessageCount = 0;

  ngAfterViewChecked(): void {
    this.scrollToBottom();
  }

  sendMessage() {
    this.messageService
      .sendMessage(this.username(), this.messageContent)
      .then(() => {
        this.messageForm?.reset();
        this.scrollToBottom();
      });
  }

  private scrollToBottom() {
    if (
      !this.scrollContainer ||
      this.previousMessageCount === this.messages().length
    ) {
      return;
    }

    this.scrollContainer.nativeElement.scrollTop =
      this.scrollContainer.nativeElement.scrollHeight;
    this.previousMessageCount = this.messages().length;
  }
}
