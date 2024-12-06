import { mount } from "@vue/test-utils";
import { describe, it, expect, beforeEach, afterEach } from "vitest";
import UploadPost from "@/pages/upload-post/UploadPost.vue";
import TagSelectionPage from "@/pages/upload-post/TagSelectionPage.vue";

describe("UploadPost.vue State Synchronization Tests", () => {
  let wrapper;

  // Mock file data
  const mockFile = new File(["test"], "test.png", { type: "image/png" });
  const mockFileList = [mockFile];

  beforeEach(() => {
    wrapper = mount(UploadPost, {
      global: {
        stubs: {
          "router-link": true,
          "router-view": true,
        },
      },
    });
  });

  afterEach(() => {
    wrapper.unmount();
  });

  it("persists data when navigating from the first page to tag selection and back", async () => {
    // Arrange: Set initial data
    const testTitle = "Test Title";
    const testDescription = "Test Description";

    await wrapper.setData({
      title: testTitle,
      description: testDescription,
      selectedFiles: mockFileList,
    });

    // Act: Transition to tag selection
    await wrapper.vm.handleSubmit();

    // Assert: Ensure data persists in tag selection state
    expect(wrapper.vm.showTagSelection).toBe(true);
    expect(wrapper.vm.title).toBe(testTitle);
    expect(wrapper.vm.description).toBe(testDescription);
    expect(wrapper.vm.selectedFiles).toEqual(mockFileList);

    // Act: Navigate back to the first page
    const tagSelection = wrapper.findComponent(TagSelectionPage);
    await tagSelection.vm.$emit("goBack");

    // Assert: Ensure data persists after navigating back
    expect(wrapper.vm.title).toBe(testTitle);
    expect(wrapper.vm.description).toBe(testDescription);
    expect(wrapper.vm.selectedFiles).toEqual(mockFileList);
  });
});
