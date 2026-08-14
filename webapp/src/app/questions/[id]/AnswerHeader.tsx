import {Label, ListBox, Select} from "@heroui/react";

type Props={
    answerCount: number
}

export default function AnswerHeader({answerCount}: Props) {
    return (
        <div className='flex items-center justify-between pt-3 w-full px-6'>
            <div className='text-2xl'>
                {answerCount} {answerCount === 1 ? 'Answer' : 'Answers'}
            </div>
            <div className='flex items-center justify-end gap-3 w-[50%] ml-auto'>
                <Select className="w-[256px]" placeholder="Select one" defaultSelectedKey="highscore">
                    <Label>Sort By</Label>
                    <Select.Trigger>
                        <Select.Value />
                        <Select.Indicator />
                    </Select.Trigger>
                    <Select.Popover>
                        <ListBox>
                            <ListBox.Item id="highscore" textValue="Highest score(Default)">
                                Highest score(Default)
                                <ListBox.ItemIndicator />
                            </ListBox.Item>
                            <ListBox.Item id="created" textValue="Date Created">
                                Date Created
                                <ListBox.ItemIndicator />
                            </ListBox.Item>
                            
                        </ListBox>
                    </Select.Popover>
                </Select>
            </div>
        </div>
    );
}
