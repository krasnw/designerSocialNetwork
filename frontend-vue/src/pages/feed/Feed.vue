<script>
import Spinner from '@/assets/Icons/Spinner.vue';
import PostView from '@/components/PostView.vue';
import { postsContentService } from '@/services/postsContent';

export default {
  components: {
    PostView,
    Spinner
  },
  data() {
    return {
      posts: [],
      currentPage: 1,
      loading: false,
      hasMore: true,
      scrollTimeout: null
    }
  },
  methods: {
    async loadPosts() {
      if (this.loading || !this.hasMore) return;

      this.loading = true;
      try {
        console.log('Loading page:', this.currentPage);
        const newPosts = await postsContentService.getFeedPosts(this.currentPage);
        console.log('Received posts:', newPosts);

        if (!Array.isArray(newPosts) || newPosts.length === 0) {
          console.log('No more posts available');
          this.hasMore = false;
        } else {
          this.posts.push(...newPosts);
          this.currentPage++;
        }
      } catch (error) {
        console.error('Error loading posts:', error);
        this.hasMore = false;
      } finally {
        this.loading = false;
      }
    },
    setupIntersectionObserver() {
      this.observer = new IntersectionObserver(
        (entries) => {
          const target = entries[0];
          if (target.isIntersecting && !this.loading && this.hasMore) {
            console.log('Sentinel is visible, loading more posts...');
            this.loadPosts();
          }
        },
        {
          rootMargin: '300px', // Увеличиваем отступ для более раннего срабатывания
          threshold: 0.1 // Немного увеличиваем порог видимости
        }
      );

      // Добавляем проверку наличия элемента
      if (this.$refs.sentinel) {
        this.observer.observe(this.$refs.sentinel);
      } else {
        console.error('Sentinel element not found');
      }
    }
  },
  async mounted() {
    await this.loadPosts();
    // Даем время для рендеринга DOM перед установкой observer
    this.$nextTick(() => {
      this.setupIntersectionObserver();
    });
  },
  beforeUnmount() {
    if (this.observer) {
      this.observer.disconnect();
    }
  },
  name: "FeedPage",
};
</script>

<template>
  <main>
    <h2 class="page-name">Strona główna</h2>
    <section class="posts" ref="postsContainer">
      <div v-if="posts.length === 0 && !loading" class="no-posts">
        Brak dostępnych postów
      </div>
      <article v-for="post in posts" :key="post.id">
        <PostView :post="post" />
      </article>
      <div ref="sentinel" class="sentinel" style="height: 20px;"></div>
      <div v-if="loading" class="loading">Ładowanie
        <Spinner class="spinner" />
      </div>
      <div v-if="!hasMore && posts.length > 0" class="no-more">
        Nie ma więcej postów
      </div>
    </section>
  </main>
</template>

<style scoped>
.posts {
  display: flex;
  flex-direction: column;
  gap: 50px;
  padding-bottom: 60px;
  min-height: 200px;
  height: 100%;
  overflow-y: auto;
  position: relative;
}

.loading,
.no-more {
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
  height: 20px;
}
</style>