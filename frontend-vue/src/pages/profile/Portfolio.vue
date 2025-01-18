<script>
import Spinner from '@/assets/Icons/Spinner.vue';
import PostPreview from './PostPreview.vue';
import { miniPostsContentService } from '@/services/postsContent';
import { userService } from "@/services/user";

export default {
  name: 'Portfolio',
  components: {
    PostPreview,
    Spinner
  },
  data() {
    return {
      userPosts: [],
      isLoading: true
    }
  },
  async created() {
    const username = this.$route.params.username || (await userService.getMyData()).username;
    this.userPosts = await miniPostsContentService.getPortfolioPosts(username).finally(() => {
      this.isLoading = false;
    });
  }
}
</script>

<template>
  <main>
    <h2 class="page-name">Portfolio</h2>
    <div v-if="isLoading" class="loading">Ładowanie
      <Spinner class="spinner" />
    </div>
    <div v-else-if="userPosts.length === 0" class="no-posts">
      Brak dostępnych postów
    </div>
    <section v-else class="posts">
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

.loading {
  font-size: 14px;
  font-weight: 600;
  color: var(--info-text-color);
  background-color: var(--element-dark-color);
  border: 0.5px solid var(--element-light-color);
  padding: 20px;
  border-radius: 10px;
  backdrop-filter: blur(10px);
  display: flex;
  flex-direction: row;
  justify-content: center;
  align-items: center;
  width: max-content;
  gap: 10px;
}

.no-posts {
  font-size: 20px;
  font-weight: 600;
  color: var(--info-text-color);
  background-color: var(--element-dark-color);
  border: 0.5px solid var(--element-light-color);
  padding: 40px 80px;
  border-radius: 10px;
  backdrop-filter: blur(10px);
  width: max-content;
}
</style>