<script>
import PostView from '@/components/PostView.vue';

export default {
  components: {
    PostView
  },
  data() {
    return {
      posts: [
        {
          username: 'Takumi Fujiwara',
          userProfilePicture: 'https://placehold.co/300x300',
          postPictures: [
            'https://placehold.co/600x500',
            'https://placehold.co/600x500',
            'https://placehold.co/600x500',
            'https://placehold.co/600x500',
            'https://placehold.co/600x500',
            'https://placehold.co/600x500',
            'https://placehold.co/600x500',
            'https://placehold.co/600x500',
            'https://placehold.co/600x500',
            'https://placehold.co/600x500'
          ],
          title: 'Parowanie czcionek',
          description: `Oto kilka par czcionek, które możesz wykorzystać w swoim projekcie. \n\nNie zapomnij zasubskrybować mojego kanału i nacisnąć przycisk Lubię to`,
          tags: ['Czcionka', 'Flat', 'Układ'],
          shares: 2739,
          likes: 35610,
          isPrivate: true
        },
        {
          username: 'Paweł Topski',
          userProfilePicture: 'https://placehold.co/300x300',
          postPictures: [
            'https://placehold.co/600x500',
            'https://placehold.co/600x500'
          ],
          title: 'Przycisk w stylu Glassmorphism',
          description: `Very long text for the description. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nulla nec purus feugiat, molestie ipsum et, ultricies turpis. Incididunt quis amet mollit aute id qui veniam ad qui incididunt deserunt ipsum irure deserunt. Fugiat excepteur dolore qui fugiat nulla tempor tempor velit magna ut. Amet Lorem non velit consectetur veniam excepteur reprehenderit cillum laborum sit dolor commodo mollit.

        Minim consectetur consequat in esse incididunt quis voluptate sit anim quis minim sint dolore dolor. Ex non eiusmod laboris dolor. Aliquip dolore incididunt qui aliqua excepteur. Anim adipisicing exercitation nulla mollit sint officia laboris. Id exercitation ipsum ut laboris enim ut. In commodo dolor nostrud consectetur enim minim velit excepteur nulla velit cillum.

        Dolore ad esse nostrud nostrud officia commodo. Pariatur incididunt veniam non quis. Qui tempor veniam ullamco sit magna dolore quis est excepteur ex dolore dolore sit. Laboris non sint deserunt laboris est et exercitation Lorem voluptate voluptate eiusmod.`,
          tags: ['Flat', 'Czerwony', 'Glassmorphism', 'Przycisk'],
          shares: 10,
          likes: 150,
          isPrivate: false
        },
        {
          username: 'Jan Kowalski',
          userProfilePicture: 'https://placehold.co/300x300',
          postPictures: [
            // 'https://placehold.co/600x500',
            'https://placehold.co/600x500'
          ],
          title: 'Sample Post Title',
          description: `Long text for the description. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nulla nec purus feugiat, molestie ipsum et, ultricies turpis. 
          Incididunt quis amet mollit aute id qui veniam ad qui incididunt deserunt ipsum irure deserunt.`,
          tags: ['Ciemny', 'Neomorphism', 'Input'],
          shares: 1345,
          likes: 15345,
          isPrivate: false
        },
      ]
    }
  },
  name: "FeedPage",
};
</script>

<template>
  <main>
    <h2 class="page-name">Strona główna</h2>
    <section class="posts">
      <article v-for="post in posts">
        <PostView :post="post" />
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