<script>
import UserBadge from './UserBadge.vue';
import userReportService from '../../services/users';

export default {
  name: 'UserView',
  components: {
    UserBadge,
  },
  props: {
    userReport: {
      type: Object,
      required: true,
    },
  },
  methods: {
    formatDate(date) {
      return new Date(date).toLocaleDateString();
    },
    async dismissReport() {
      await userReportService.dismissReport(this.userReport.id);
    },
    async freezeUser() {
      await userReportService.toggleFrozenUser(this.userReport.reportedUser.username);
    }
  }
}
</script>

<template>
  <article class="flex flex-col gap-4 bg-neutral-900 rounded-lg px-4 py-3">
    <div class="flex flex-row gap-4">
      <span class="flex flex-col gap-2">
        <p>Użytkownik reklamowany</p>
        <UserBadge :user="userReport.reportedUser" />
      </span>
      <span class="flex flex-col gap-2">
        <p>Użytkownik zgłaszający</p>
        <UserBadge :user="userReport.reporter" />
      </span>
    </div>
    <div class="flex flex-row gap-2 justify-between items-center">
      <div class="flex flex-col gap-2">
        <p>{{ userReport.reportReason }}</p>
        <p class="max-w-96">{{ userReport.description }}</p>
        <p>{{ formatDate(userReport.reportDate) }}</p>
      </div>
      <div class="flex flex-col gap-2">
        <button class="accept-button" @click="dismissReport">Anuluj</button>
        <button class="reject-button" @click="freezeUser">Zmróź</button>
      </div>
    </div>
  </article>
</template>