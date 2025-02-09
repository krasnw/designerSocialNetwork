<script>
import PostView from '../views/PostView.vue';
import postReportService from '../../services/posts';

export default {
  name: 'PostsReports',
  data() {
    return {
      isLoading: true,
      reports: [],
    }
  },
  components: {
    PostView,
  },
  async created() {
    this.isLoading = true;
    this.reports = await postReportService.fetchPostReport();
    this.isLoading = false;
  }
}
</script>

<template>
  <main v-if="!isLoading" class="flex flex-col gap-4 items-center">
    <PostView v-for="post in reports" :key="post.id" :post="post" />
  </main>
</template>