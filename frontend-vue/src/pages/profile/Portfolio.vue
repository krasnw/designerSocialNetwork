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
      isLoading: false,
      currentPage: 1,
      hasMore: true,
      observer: null
    }
  },
  methods: {
    async loadPosts(page) {
      if (!this.hasMore || this.isLoading) return;

      this.isLoading = true;
      try {
        const posts = this.$route.params.username
          ? await miniPostsContentService.getPortfolioPosts(this.$route.params.username, page)
          : await miniPostsContentService.getMyMiniPosts(page);

        if (posts.message === "No posts found." || (Array.isArray(posts) && posts.length === 0)) {
          this.hasMore = page > 1;
          return;
        }

        this.userPosts.push(...posts);
        this.currentPage++;
      } catch (error) {
        console.error('Failed to load posts:', error);
      } finally {
        this.isLoading = false;
      }
    },
    setupObserver() {
      this.observer = new IntersectionObserver(
        (entries) => {
          if (entries[0].isIntersecting) {
            this.loadPosts(this.currentPage);
          }
        },
        { threshold: 0.5 }
      );

      const sentinel = this.$refs.sentinel;
      if (sentinel) {
        this.observer.observe(sentinel);
      }
    }
  },
  async created() {
    await this.loadPosts(this.currentPage);
  },
  mounted() {
    this.setupObserver();
  },
  beforeUnmount() {
    if (this.observer) {
      this.observer.disconnect();
    }
  }
}
</script>

<template>
  <main>
    <h2 class="page-name">Portfolio</h2>
    <section class="posts">
      <PostPreview v-for="post in userPosts" :key="post.title" :post="post" />
    </section>
    <div v-if="hasMore" ref="sentinel" class="sentinel">
      <Spinner v-if="isLoading" class="spinner" />
    </div>
    <div v-else-if="userPosts.length === 0" class="no-posts">
      No posts available
    </div>
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

.sentinel {
  width: 100%;
  height: 50px;
  display: flex;
  justify-content: center;
  align-items: center;
  margin: 20px 0;
}

.spinner {
  width: 30px;
  height: 30px;
}
</style>