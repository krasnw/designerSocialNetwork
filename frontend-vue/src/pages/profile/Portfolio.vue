<script>
import PostPreview from './PostPreview.vue';
import { miniPostsContentService } from '@/services/postsContent';
import { userService } from "@/services/user";

export default {
  name: 'Portfolio',
  components: {
    PostPreview
  },
  data() {
    return {
      userPosts: [],
    }
  },
  async created() {
    const username = this.$route.params.username || (await userService.getMyData()).username;
    this.userPosts = await miniPostsContentService.getPortfolioPosts(username);
  }
}
</script>

<template>
  <main>
    <h2 class="page-name">Portfolio</h2>
    <section class="posts">
      <PostPreview v-for="post in userPosts" :key="post.title" :post="post" />
    </section>
  </main>
</template>

<style scoped>
.posts {
  display: flex;
  flex-wrap: wrap;
  gap: 20px;
  margin-bottom: 60px;
}
</style>