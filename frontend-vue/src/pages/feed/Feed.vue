<script>
import PostView from '@/components/PostView.vue';
import PostPreview from '../profile/PostPreview.vue';

export default {
  components: {
    PostPreview,
    PostView
  },
  data() {
    return {
      testData: {
        username: 'John Doe',
        userProfilePicture: 'https://placehold.co/600x500',
        postPitures: [
          'https://placehold.co/600x500',
          'https://placehold.co/600x500'
        ],
        title: 'Sample Post Title',
        description: 'This is a sample description for the post.',
        tags: ['tag1', 'tag2', 'tag3'],
        shares: 10,
        likes: 150
      },
      posts: [
        {
          title: 'Post title lorem ipsum dolor sit amet. Post title lorem ipsum dolor sit amet.',
          image: 'https://placehold.co/600x600',
          like: 3442,
          isLiked: true
        },
        {
          title: 'Post title lorem ipsum dolor sit amet. Post title lorem ipsum dolor sit amet.',
          image: 'https://placehold.co/600x500',
          like: 2334,
          isLiked: false
        },
        {
          title: 'Post title lorem ipsum dolor sit amet. Post title lorem ipsum dolor sit amet.',
          image: 'https://placehold.co/600x500',
          like: 599332,
          isLiked: true
        }
      ]
    }
  },
  name: "FeedPage",
};
</script>

<template>
  <main>
    <h2 class="page-name">Strona główna</h2>
    <PostView :username="testData.username" :userProfilePicture="testData.userProfilePicture"
      :postPictures="testData.postPictures" :title="testData.title" :description="testData.description"
      :tags="testData.tags" :shares="testData.shares" :likes="testData.likes" />
    <section class="posts">
      <article v-for="(post, index) in posts" :key="index">
        <PostPreview :post="post" />
      </article>
    </section>
  </main>
</template>

<style scoped>
.posts {
  display: flex;
  flex-direction: column;
  gap: 50px;
  padding-bottom: 60px;
}
</style>