<script>
import defaultImage from '../../assets/avatar.png';
import { imageDirectory } from '../../services/constants';

export default {
  name: 'UserBadge',
  props: {
    user: {
      type: Object,
      required: true,
    },
  },
  methods: {
    imageHandler(image) {
      return image ? imageDirectory + image : defaultImage;
    },
    formatTextToHTML(text) {
      if (!text) return 'No description';
      if (text.length === 0) return 'No description';
      return text.replace(/\n/g, '<br>');
    },
  }
}
</script>

<template>
  <div class="flex flex-col gap-4 bg-neutral-700 p-4 max-w-max rounded-lg">
    <div class="flex items-center gap-4 bg-neutral-800 p-4 rounded-lg max-w-xl">
      <img :src="imageHandler(user.profileImage)" alt="User profile image" class="w-16 h-16 rounded-full">
      <div>
        <h2 class="text-xl font-semibold">{{ user.username }}</h2>
        <p class="text-gray-500">{{ user.firstName }} {{ user.lastName }}</p>
      </div>
    </div>
    <p v-html="formatTextToHTML(user.profileDescription)" class="max-w-xl text-balance bg-neutral-800 p-4 rounded-lg">
    </p>
  </div>
</template>