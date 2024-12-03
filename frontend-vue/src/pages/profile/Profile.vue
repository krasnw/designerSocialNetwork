<script>
import PostPreview from './PostPreview.vue';

export default {
  name: 'Profile',
  components: {
    PostPreview
  },
  data() {
    return {
      userPosts: [
        {
          title: 'Post 1',
          image: 'https://placehold.co/600x600',
          like: 134452
        },
        {
          title: 'Post 2',
          image: 'https://placehold.co/600x600',
          like: 12341
        },
        {
          title: 'Post 3',
          image: 'https://placehold.co/600x600',
          like: 76354
        },
        {
          title: 'Post 4',
          image: 'https://placehold.co/600x600',
          like: 23476
        },
        {
          title: 'Post 5',
          image: 'https://placehold.co/600x600',
          like: 25471
        },
        {
          title: 'Post 6',
          image: 'https://placehold.co/600x600',
          like: 1577
        },
        {
          title: 'Post 7',
          image: 'https://placehold.co/600x600',
          like: 62824
        },
      ]
    }
  }
}
</script>

<template>
  <main>
    <h2 class="page-name">Posty</h2>
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