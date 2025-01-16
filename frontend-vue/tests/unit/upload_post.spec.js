import { shallowMount } from "@vue/test-utils";
import UploadPost from "@/pages/upload-post/UploadPost.vue";

describe("UploadPost.vue", () => {
  let wrapper;

  beforeEach(() => {
    wrapper = shallowMount(UploadPost, {
      data() {
        return {
          title: "",
          description: "",
          selectedFiles: [],
          dragActive: false,
          showTagSelection: false,
          showErrors: false,
          mainPhotoIndex: 0,
          tagInputGather: () => {},
        };
      },
    });
  });

  it("validates empty form as invalid", () => {
    expect(wrapper.vm.isFormValid).toBe(false);
  });

  it("validates form with only title as invalid", () => {
    wrapper.setData({ title: "Test Title" });
    expect(wrapper.vm.isFormValid).toBe(false);
  });

  it("validates form with only files as invalid", () => {
    wrapper.setData({ selectedFiles: [new File([""], "test.jpg")] });
    expect(wrapper.vm.isFormValid).toBe(false);
  });

  it("validates form with title and files as valid", () => {
    wrapper.setData({
      title: "Test Title",
      selectedFiles: [new File([""], "test.jpg")],
    });
    expect(wrapper.vm.isFormValid).toBe(true);
  });

  it("shows error states when submitting invalid form", async () => {
    await wrapper.vm.handleSubmit();
    expect(wrapper.vm.showErrors).toBe(true);
  });

  it("limits file upload to maximum of 10 files", () => {
    const files = Array(11).fill(new File([""], "test.jpg"));
    const event = {
      target: {
        files: files,
      },
    };

    global.alert = jest.fn();
    wrapper.vm.handleFileChange(event);

    expect(wrapper.vm.selectedFiles.length).toBe(0);
    expect(global.alert).toHaveBeenCalledWith(
      "Możesz dodać maksymalnie 10 zdjęć"
    );
  });

  it("allows adding files up to 10", () => {
    const files = Array(5).fill(new File([""], "test.jpg"));
    const event = {
      target: {
        files: files,
      },
    };

    wrapper.vm.handleFileChange(event);
    expect(wrapper.vm.selectedFiles.length).toBe(5);

    const moreFiles = Array(3).fill(new File([""], "test.jpg"));
    const secondEvent = {
      target: {
        files: moreFiles,
      },
    };

    wrapper.vm.handleFileChange(secondEvent);
    expect(wrapper.vm.selectedFiles.length).toBe(8);
  });

  it("proceeds to tag selection when form is valid", async () => {
    wrapper.setData({
      title: "Test Title",
      selectedFiles: [new File([""], "test.jpg")],
    });

    await wrapper.vm.handleSubmit();
    expect(wrapper.vm.showTagSelection).toBe(true);
    expect(wrapper.vm.showErrors).toBe(false);
  });
});
