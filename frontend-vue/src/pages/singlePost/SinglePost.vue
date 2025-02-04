<script>
import Spinner from '@/assets/Icons/Spinner.vue';
import PostView from '@/components/PostView.vue';
import { postsContentService } from '@/services/postsContent';

export default {
  name: 'SinglePost',
  data() {
    return {
      isLoading: true,
      post: null,
      error: null,
      username: '',
      gradientClasses: ['green-gradient', 'red-gradient', 'blue-gradient']
    }
  },
  components: {
    PostView,
    Spinner
  },
  computed: {
    randomGradientClass() {
      return this.gradientClasses[Math.floor(Math.random() * this.gradientClasses.length)]
    }
  },
  methods: {
    async fetchPost() {
      try {
        const routeName = this.$route.name;
        if (routeName === 'protectedPost') {
          const hash = this.$route.params.hash;
          this.post = await postsContentService.getProtectedPost(hash);
        } else {
          const postId = this.$route.params.id;
          this.post = await postsContentService.getPost(postId);
        }
        this.username = this.post.author.username;
      } catch (error) {
        this.$router.push(`/error/${error.status}`);
      } finally {
        this.isLoading = false;
      }
    }
  },
  mounted() {
    this.fetchPost();
  }
}
</script>

<template>
  <main>
    <h2 class="page-name">Post użytkownika <span :class="randomGradientClass" class="text-shadow">{{ username }}</span>
    </h2>
    <div v-if="isLoading" class="loading">Ładowanie
      <Spinner class="spinner" />
    </div>
    <PostView v-else-if="post" :post="post" />
  </main>
</template>

<style scoped>
.text-shadow {
  text-shadow: 0 0 10px rgba(255, 255, 255, 0.5);
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
  gap: 15px;
  flex-direction: row;
  justify-content: center;
  align-items: center;
  width: max-content;
}
</style>